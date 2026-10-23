import {
    FormEvent,
    useCallback,
    useEffect,
    useState
} from 'react';

import { PageTitle } from '../components/UI';
import { api } from '../api/client';

interface UserRow {
    id: string;
    fullName: string;
    email: string;
    role: string;
    isActive: boolean;
}

interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

interface CreateUserForm {
    fullName: string;
    email: string;
    role: string;
    password: string;
}

const initialForm: CreateUserForm = {
    fullName: '',
    email: '',
    role: 'CreditTeam',
    password: ''
};

export default function Admin() {
    const [rows, setRows] = useState<UserRow[]>([]);
    const [roles, setRoles] = useState<string[]>([]);

    const [search, setSearch] = useState('');

    const [show, setShow] = useState(false);

    const [form, setForm] =
        useState<CreateUserForm>(initialForm);

    // Pagination
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);

    const [totalCount, setTotalCount] = useState(0);
    const [totalPages, setTotalPages] = useState(0);

    const [hasPreviousPage, setHasPreviousPage] =
        useState(false);

    const [hasNextPage, setHasNextPage] =
        useState(false);

    // UI state
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    /**
     * Load paginated users from the backend.
     */
    const load = useCallback(async () => {
        setLoading(true);
        setError('');

        try {
            const response =
                await api.get<PagedResult<UserRow>>(
                    '/users',
                    {
                        params: {
                            search: search.trim() || undefined,
                            pageNumber,
                            pageSize
                        }
                    }
                );

            const data = response.data;

            setRows(data.items ?? []);

            setTotalCount(
                data.totalCount ?? 0
            );

            setTotalPages(
                data.totalPages ?? 0
            );

            setHasPreviousPage(
                data.hasPreviousPage ?? false
            );

            setHasNextPage(
                data.hasNextPage ?? false
            );
        }
        catch (err: any) {
            console.error(
                'Failed to load users:',
                err
            );

            setRows([]);

            setError(
                err.response?.data?.error ??
                'Unable to load users.'
            );
        }
        finally {
            setLoading(false);
        }
    }, [
        search,
        pageNumber,
        pageSize
    ]);

    /**
     * Load roles once.
     */
    useEffect(() => {
        const loadRoles = async () => {
            try {
                const response =
                    await api.get<string[]>(
                        '/users/roles'
                    );

                setRoles(response.data);
            }
            catch (err) {
                console.error(
                    'Failed to load roles:',
                    err
                );
            }
        };

        loadRoles();
    }, []);

    /**
     * Reload whenever page/page-size changes.
     *
     * Search itself does not automatically call the API
     * until the user submits the search.
     */
    useEffect(() => {
        load();
    }, [
        pageNumber,
        pageSize
    ]);

    /**
     * Search all users through the backend.
     */
    function handleSearch(
        e: FormEvent
    ) {
        e.preventDefault();

        if (pageNumber !== 1) {
            setPageNumber(1);
        }
        else {
            load();
        }
    }

    /**
     * Clear search and return to page 1.
     */
    function clearSearch() {
        setSearch('');

        if (pageNumber !== 1) {
            setPageNumber(1);
        }
        else {
            // Because setSearch is asynchronous,
            // explicitly request without a search term.
            loadWithoutSearch();
        }
    }

    async function loadWithoutSearch() {
        setLoading(true);
        setError('');

        try {
            const response =
                await api.get<PagedResult<UserRow>>(
                    '/users',
                    {
                        params: {
                            pageNumber: 1,
                            pageSize
                        }
                    }
                );

            const data = response.data;

            setRows(data.items ?? []);
            setTotalCount(data.totalCount ?? 0);
            setTotalPages(data.totalPages ?? 0);

            setHasPreviousPage(
                data.hasPreviousPage ?? false
            );

            setHasNextPage(
                data.hasNextPage ?? false
            );
        }
        catch (err: any) {
            setError(
                err.response?.data?.error ??
                'Unable to load users.'
            );
        }
        finally {
            setLoading(false);
        }
    }

    /**
     * Create new user.
     */
    async function create(
        e: FormEvent
    ) {
        e.preventDefault();

        setError('');

        try {
            await api.post(
                '/users',
                form
            );

            setShow(false);

            setForm({
                ...initialForm,
                role:
                    roles[0] ??
                    'CreditTeam'
            });

            // Newly-created user may change
            // the total record count.
            if (pageNumber !== 1) {
                setPageNumber(1);
            }
            else {
                await load();
            }
        }
        catch (err: any) {
            setError(
                err.response?.data?.error ??
                'Unable to create user.'
            );
        }
    }

    /**
     * Enable/disable user.
     */
    async function toggle(
        user: UserRow
    ) {
        setError('');

        try {
            await api.patch(
                `/users/${user.id}/status`,
                {
                    isActive:
                        !user.isActive
                }
            );

            await load();
        }
        catch (err: any) {
            setError(
                err.response?.data?.error ??
                'Unable to update user status.'
            );
        }
    }

    /**
     * Permanently delete user.
     */
    async function del(
        id: string
    ) {
        const confirmed =
            window.confirm(
                'Permanently delete this user?'
            );

        if (!confirmed) {
            return;
        }

        setError('');

        try {
            await api.delete(
                `/users/${id}`
            );

            /*
             * Example:
             * Page 2 contains one user.
             * Delete that user.
             * We should move back to page 1 instead
             * of leaving the UI on an empty page 2.
             */
            if (
                rows.length === 1 &&
                pageNumber > 1
            ) {
                setPageNumber(
                    current => current - 1
                );
            }
            else {
                await load();
            }
        }
        catch (err: any) {
            setError(
                err.response?.data?.error ??
                'Unable to delete user.'
            );
        }
    }

    function previousPage() {
        if (
            hasPreviousPage &&
            !loading
        ) {
            setPageNumber(
                current =>
                    Math.max(
                        current - 1,
                        1
                    )
            );
        }
    }

    function nextPage() {
        if (
            hasNextPage &&
            !loading
        ) {
            setPageNumber(
                current =>
                    current + 1
            );
        }
    }

    return (
        <>
            <PageTitle
                title="User Administration"
                subtitle="Register users, assign workflow roles and manage access."
                action={
                    <button
                        className="primary small"
                        onClick={() =>
                            setShow(
                                current => !current
                            )
                        }
                    >
                        + Register User
                    </button>
                }
            />

            {error && (
                <div className="alert">
                    {error}
                </div>
            )}

            {show && (
                <form
                    className="panel formPanel"
                    onSubmit={create}
                >
                    <h2>
                        Register User
                    </h2>

                    <div className="formGrid">
                        <label>
                            Full Name

                            <input
                                required
                                value={form.fullName}
                                onChange={e =>
                                    setForm({
                                        ...form,
                                        fullName:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label>
                            Business Email

                            <input
                                required
                                type="email"
                                value={form.email}
                                onChange={e =>
                                    setForm({
                                        ...form,
                                        email:
                                            e.target.value
                                    })
                                }
                            />
                        </label>

                        <label>
                            Role

                            <select
                                value={form.role}
                                onChange={e =>
                                    setForm({
                                        ...form,
                                        role:
                                            e.target.value
                                    })
                                }
                            >
                                {roles.map(role => (
                                    <option
                                        key={role}
                                        value={role}
                                    >
                                        {role}
                                    </option>
                                ))}
                            </select>
                        </label>

                        <label>
                            Temporary Password

                            <input
                                required
                                type="password"
                                value={form.password}
                                onChange={e =>
                                    setForm({
                                        ...form,
                                        password:
                                            e.target.value
                                    })
                                }
                            />
                        </label>
                    </div>

                    <button
                        className="primary small"
                        type="submit"
                    >
                        Create User
                    </button>
                </form>
            )}

            <div className="panel">
                <div className="panelHead">
                    <div>
                        <h2>
                            Registered Users
                        </h2>

                        <p>
                            Active and inactive
                            staff accounts.
                        </p>
                    </div>

                    <form
                        className="searchBox"
                        onSubmit={handleSearch}
                    >
                        <input
                            placeholder="Search name or email"
                            value={search}
                            onChange={e =>
                                setSearch(
                                    e.target.value
                                )
                            }
                        />

                        <button
                            type="submit"
                            disabled={loading}
                        >
                            Search
                        </button>

                        {search && (
                            <button
                                type="button"
                                className="linkBtn"
                                onClick={clearSearch}
                            >
                                Clear
                            </button>
                        )}
                    </form>
                </div>

                <div className="tableWrap">
                    <table>
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>

                        <tbody>
                            {loading ? (
                                <tr>
                                    <td colSpan={5}>
                                        Loading users...
                                    </td>
                                </tr>
                            ) : rows.length === 0 ? (
                                <tr>
                                    <td colSpan={5}>
                                        No users found.
                                    </td>
                                </tr>
                            ) : (
                                rows.map(user => (
                                    <tr key={user.id}>
                                        <td>
                                            <b>
                                                {user.fullName}
                                            </b>
                                        </td>

                                        <td>
                                            {user.email}
                                        </td>

                                        <td>
                                            {user.role}
                                        </td>

                                        <td>
                                            <span
                                                className={
                                                    `badge ${user.isActive
                                                        ? 'active'
                                                        : 'inactive'
                                                    }`
                                                }
                                            >
                                                {user.isActive
                                                    ? 'Active'
                                                    : 'Inactive'}
                                            </span>
                                        </td>

                                        <td>
                                            <button
                                                type="button"
                                                className="dangerLink"
                                                onClick={() =>
                                                    toggle(user)
                                                }
                                            >
                                                {user.isActive
                                                    ? 'Disable'
                                                    : 'Enable'}
                                            </button>

                                            {' '}

                                            <button
                                                type="button"
                                                className="dangerLink"
                                                onClick={() =>
                                                    del(user.id)
                                                }
                                            >
                                                Delete
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>

                <div className="pagination">
                    <div>
                        Showing{' '}
                        <b>{rows.length}</b>
                        {' '}of{' '}
                        <b>{totalCount}</b>
                        {' '}users
                    </div>

                    <div>
                        <button
                            type="button"
                            className="linkBtn"
                            disabled={
                                !hasPreviousPage ||
                                loading
                            }
                            onClick={
                                previousPage
                            }
                        >
                            Previous
                        </button>

                        <span>
                            Page{' '}
                            <b>
                                {totalPages === 0
                                    ? 0
                                    : pageNumber}
                            </b>
                            {' '}of{' '}
                            <b>
                                {totalPages}
                            </b>
                        </span>

                        <button
                            type="button"
                            className="linkBtn"
                            disabled={
                                !hasNextPage ||
                                loading
                            }
                            onClick={
                                nextPage
                            }
                        >
                            Next
                        </button>

                        <select
                            value={pageSize}
                            disabled={loading}
                            onChange={e => {
                                setPageSize(
                                    Number(
                                        e.target.value
                                    )
                                );

                                setPageNumber(1);
                            }}
                        >
                            <option value={5}>
                                5 per page
                            </option>

                            <option value={10}>
                                10 per page
                            </option>

                            <option value={20}>
                                20 per page
                            </option>

                            <option value={50}>
                                50 per page
                            </option>
                        </select>
                    </div>
                </div>
            </div>
        </>
    );
}