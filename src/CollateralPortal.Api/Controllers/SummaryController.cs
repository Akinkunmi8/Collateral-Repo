//using CollateralPortal.Api.Authorization;
//using CollateralPortal.Application.Collaterals.Queries.GetCollateralSummary;
//using CollateralPortal.Infrastructure.Persistence;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace CollateralPortal.Api.Controllers;
//[ApiController]
//[Route("api/summary")]
//[Authorize]
//public class SummaryController : ControllerBase
//{
//    private readonly IMediator _mediator;

//    public SummaryController(IMediator mediator) => _mediator = mediator;
//    [HttpGet("summary")]
//    public async Task<ActionResult<CollateralSummaryVm>> GetSummary(CancellationToken cancellationToken)
//    {
//        var result = await _mediator.Send(new GetCollateralSummaryQuery(),cancellationToken);

//        return Ok(result);
//    }
//}
