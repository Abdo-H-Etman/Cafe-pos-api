# Cafe Point Of Sale (POS) System

# Roles
- **Admin**:
    - Can do any thing in the system.
- **Manager**:
    - Can list, update, and delete users in his within his branch only.
- **Cashier**:
    - Can only create orders in his branch

# Models
- **Role**:
  - Fields: `id`, `name`
  - values: `Admin` can create, delete users, `Cashier` can create, update orders
- **Branch**
  - Fields: `id`, `name`, `address`
- **User**
  - Fields: `id`, `email`, `name`, `username`, `branchId`, `roleId`

### API Scheme
#### Common Responses
- **400 Bad Request**: Incorrect request scheme.
- **401 Forbidden**: User not permitted to perform the operation.
- **403 Unauthorized**: Anonymous user, redirect to login.
- **500 Internal Server Error**: Server-side error.

# Auth Endpoints
| Endpoint | Method | Description | Request Body | Success Response | Error Response |
|----------|--------|-------------|--------------|------------------|----------------|
| `/api/auth/login` | POST | Obtain JWT token | `{ identifier: string, password: string }` | **200 OK**: `{ accessToken: string, refreshToken: string, expiresAt: date, user: { id: guid, name: string, email: string, userName: string, branchId: guid, dateJoined: date } }` | 400, 401, 403 | 
| `/api/auth/refresh-token` | POST | Refresh JWT token | `{ accessToken: string, refreshToken: string }` | **200 OK**: `{ accessToken: string, refreshToken: string, expiresAt: date, user: { id: guid, name: string, email: string, userName: string, branchId: guid, dateJoined: date } }` | 400, 401, 403 |
| `/api/auth/logout` | POST | Logout from the system | None | **200 OK** No Content | 400, 401, 403 |  
| `/api/auth/register` | POST | Create user (Admin and Manager only) | `{ email: string (optional), userName: string, name: string, branchId: guid, roles: [ string ], password: string, confirmPassword: string }` | **201 Created**: `{ id: guid, branch: string, roles: [ string], name: string, email: string, userName: string, branchId: guid, dateJoined: date }` | 400, 403, 500 |

# User Endpoints
| Endpoint | Method | Description | Request Body | Success Response | Error Response |
|----------|--------|-------------|--------------|------------------|----------------|
| `/api/users/{id}` | GET | Get user by id (Admin and Manager only) | None | **200 OK**: `{ id: guid, branch: string, roles: [ string], name: string, email: string, userName: string, branchId: guid, dateJoined: date }` | 403, 404, 500 |
| `/api/users/by-email` | GET | Get user by email (Admin and Manager only) email sends in query parameter "email" | None | **200 OK**: `{ id: guid, branch: string, roles: [ string], name: string, email: string, userName: string, branchId: guid, dateJoined: date }` | 403, 404, 500 |
| `/api/users/by-branch/{branchId}` | GET | List users in specific branch (Admin and the Manager of this branch only) | None | **200 OK**: `[ { id: guid, branch: string, roles: [ string], name: string, email: string, userName: string, branchId: guid, dateJoined: date } ]` | 403, 500 |
| `/api/users/by-role` | GET | List users with specific role (Admin only) | None | **200 OK**: `[ { id: guid, branch: string, roles: [ string], name: string, email: string, userName: string, branchId: guid, dateJoined: date } ]` | 403, 500 |
| `/api/users/{id}` | PATCH | Update a user (Admin and Manager only) | `{ name: string, userName: string, email: string}` all fileds optional | **201 Created**: Same as GET By Id | 400, 403, 404, 500 |
| `/api/users/{id}` | DELETE | Delete a user (Admin and Manager only) | None | **204 No Content**: Same as GET By Id | 403, 404, 500 |

