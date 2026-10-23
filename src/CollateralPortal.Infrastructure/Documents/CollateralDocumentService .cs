using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Documents;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using CollateralPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CollateralPortal.Infrastructure.Documents;

public sealed class CollateralDocumentService
    : ICollateralDocumentService
{
    private const int MaxFileSize = 10_000_000;

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/png",
            "image/jpeg"
        };

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".png",
            ".jpg",
            ".jpeg"
        };

    private readonly ApplicationDbContext _db;
    private readonly string _storageRoot;

    public CollateralDocumentService(ApplicationDbContext db,IOptions<DocumentStorageOptions> options)
    {
        _db = db;

        if (string.IsNullOrWhiteSpace(options.Value.RootPath))
        {
            throw new InvalidOperationException(
                "Document storage root path has not been configured.");
        }

        _storageRoot = Path.GetFullPath(options.Value.RootPath);

        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<IReadOnlyList<CollateralDocumentDto>> GetAllAsync(Guid collateralId,
            CancellationToken cancellationToken = default)
    {
        return await _db.CollateralDocuments
            .AsNoTracking()
            .Where(x =>
                x.CollateralId == collateralId)
            .OrderByDescending(x =>
                x.CreatedAtUtc)
            .Select(x =>
                new CollateralDocumentDto(
                    x.Id,
                    x.FileName,
                    x.DocumentType,
                    x.ContentType,
                    x.UploadedBy,
                    x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<UploadCollateralDocumentResult>UploadAsync( UploadCollateralDocumentCommand command,
            CancellationToken cancellationToken = default)
    {
        var collateralExists = await _db.Collaterals.AnyAsync(x =>x.Id == command.CollateralId,cancellationToken);

        if (!collateralExists)
        {
            throw new KeyNotFoundException(
                $"Collateral '{command.CollateralId}' was not found.");
        }

        ValidateFile(command);

        var originalFileName =
            Path.GetFileName(
                command.FileName);

        var extension =
            Path.GetExtension(
                originalFileName)
                .ToLowerInvariant();

        /*
         * Generate our own physical filename.
         *
         * Never use the client-supplied filename
         * as the physical server filename.
         */
        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        /*
         * LegalReport will naturally become:
         *
         * Collaterals/
         * {collateralId}/
         * LegalReport/
         */
        var relativeDirectory =
            Path.Combine(
                "Collaterals",
                command.CollateralId.ToString(),
                command.DocumentType.ToString());

        var physicalDirectory =
            Path.Combine(
                _storageRoot,
                relativeDirectory);

        Directory.CreateDirectory(
            physicalDirectory);

        var relativePath =
            Path.Combine(
                relativeDirectory,
                storedFileName);

        var physicalPath =
            GetSafePhysicalPath(
                relativePath);

        try
        {
            await File.WriteAllBytesAsync(physicalPath,command.Content,cancellationToken);

            var document =
                new CollateralDocument
                {
                    Id = Guid.NewGuid(),

                    CollateralId =
                        command.CollateralId,

                    DocumentType =
                        command.DocumentType,

                    FileName =
                        originalFileName,

                    ContentType =
                        command.ContentType,

                    FileSize =
                        command.Content.LongLength,

                    RelativePath =
                        relativePath,

                    /*
                     * New uploads are filesystem backed.
                     */
                    Content = null,

                    UploadedBy =
                        command.UploadedBy
                };

            _db.CollateralDocuments.Add(
                document);

            await _db.SaveChangesAsync(
                cancellationToken);

            return new UploadCollateralDocumentResult(
                document.Id);
        }
        catch
        {
            /*
             * If database persistence fails after the
             * physical file was written, remove it.
             */
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            throw;
        }
    }

    public async Task<CollateralDocumentDownloadDto?>DownloadAsync(Guid collateralId,Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document =
            await _db.CollateralDocuments
                .AsNoTracking()
                .Where(x =>
                    x.Id == documentId &&
                    x.CollateralId == collateralId)
                .Select(x => new
                {
                    x.FileName,
                    x.ContentType,
                    x.RelativePath,
                    x.Content
                })
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (document is null)
        {
            return null;
        }

        byte[] content;

        /*
         * New filesystem-backed document.
         */
        if (!string.IsNullOrWhiteSpace(document.RelativePath))
        {
            var physicalPath = GetSafePhysicalPath(document.RelativePath);

            if (!File.Exists(physicalPath))
            {
                throw new FileNotFoundException(
                    $"Document '{documentId}' exists in the database, " +
                    "but the physical file could not be found.");
            }

            content =
                await File.ReadAllBytesAsync(
                    physicalPath,
                    cancellationToken);
        }

        /*
         * Legacy document stored in SQL Server.
         */
        else if (
            document.Content is not null &&
            document.Content.Length > 0)
        {
            content =
                document.Content;
        }
        else
        {
            throw new FileNotFoundException(
                $"No file content was found for document '{documentId}'.");
        }

        return new CollateralDocumentDownloadDto(
            content,
            document.ContentType,
            document.FileName);
    }

    private static void ValidateFile(UploadCollateralDocumentCommand command)
    {
        if (command.Content is null || command.Content.Length == 0)
        {
            throw new ArgumentException(
                "The uploaded document is empty.");
        }

        if (command.Content.Length > MaxFileSize)
        {
            throw new ArgumentException(
                "The maximum permitted document size is 10 MB.");
        }
        if (command.DocumentType == CollateralDocumentType.LegalReport &&!string.Equals(command.ContentType,"application/pdf",
        StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Legal Perfection reports must be PDF documents.");
        }

        if (!AllowedContentTypes.Contains(command.ContentType))
        {
            throw new ArgumentException(
                "Only PDF, PNG and JPEG documents are allowed.");
        }

        var fileName = Path.GetFileName(command.FileName);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "A valid document filename is required.");
        }

        var extension = Path.GetExtension(fileName);

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "Only PDF, PNG, JPG and JPEG files are allowed.");
        }

        /*
         * Ensure MIME type and extension agree.
         */
        var validCombination =
            command.ContentType.ToLowerInvariant()
            switch
            {
                "application/pdf" =>
                    extension.Equals(
                        ".pdf",
                        StringComparison.OrdinalIgnoreCase),

                "image/png" =>
                    extension.Equals(
                        ".png",
                        StringComparison.OrdinalIgnoreCase),

                "image/jpeg" =>
                    extension.Equals(
                        ".jpg",
                        StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(
                        ".jpeg",
                        StringComparison.OrdinalIgnoreCase),

                _ => false
            };

        if (!validCombination)
        {
            throw new ArgumentException(
                "The document extension does not match its content type.");
        }
    }

    private string GetSafePhysicalPath(string relativePath)
    {
        var physicalPath =Path.GetFullPath(Path.Combine(_storageRoot, relativePath));

        /*
         * Protect against directory traversal.
         */
        var rootWithSeparator = _storageRoot.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!physicalPath.StartsWith(rootWithSeparator,StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid document storage path.");
        }

        return physicalPath;
    }
}