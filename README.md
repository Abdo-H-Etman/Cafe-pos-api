# Cafe Point Of Sale (POS) System


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
| `/api/auth/token` | POST | Obtain JWT token | `{ identifier: string, password: string }` | **200 OK**: `{ access: string, refresh: string, expiresAt: date, user: { id: guid, name: string, role: string } }` | 400, 401, 403 | 
| `/api/auth/token/refresh` | POST | Refresh JWT token | `{ refresh: string }` | **200 OK**: `{ access: string, refresh: string, expiresAt: date, user: { id: guid, name: string, role: string, branch: string } }` | 400, 401, 403 |

# User Endpoints
| Endpoint | Method | Description | Request Body | Success Response | Error Response |
|----------|--------|-------------|--------------|------------------|----------------|
| `/api/users` | POST | Create user (Admin only) | `{ email: string, userName: string, name: string, branchId: guid, roleId: guid }` | **201 Created**: `{ id: guid, name: string, role: string, branch: string }` | 400, 403, 500 |
| `/api/users` | GET | List all users (Admin only) | None | **200 OK**: `[ { id: guid, name: string, role: string, branch: name} ]` | 403, 500 |
| `/api/users/{id}` | PATCH | Update a user (Admin only) | Same as POST, fields optional | **201 Created**: Same as POST | 400, 403, 404, 500 |
| `/api/users/{id}` | DELETE | Delete a user (Admin only) | None | **204 No Content**: Same as POST | 403, 404, 500 |

