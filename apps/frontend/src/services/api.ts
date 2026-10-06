import type { CompanyFormInput } from '../types/company';

const apiUrl = import.meta.env.VITE_API_URL ?? 'https://localhost:5001';

export interface Session {
  accessToken: string;
  userName: string;
  role: string;
  expiresAt: string;
}
export interface CompanySummary {
  id: string;
  companyName: string;
  cnpj: string;
  stateRegistration?: string;
  updatedAt: string;
}
export interface ManagedUser {
  id: string;
  name: string;
  email: string;
  role: 'Administrator' | 'Operator';
  isActive: boolean;
}
export interface AuditEvent {
  id: string;
  actorUserId?: string;
  action:
    | 'OrganizationRegistered'
    | 'UserAuthenticated'
    | 'CompanyCreated'
    | 'CompanyCredentialsViewed'
    | 'CompanyUpdated'
    | 'CompanyDeleted'
    | 'UserCreated'
    | 'UserActivationChanged';
  entityType: string;
  entityId?: string;
  occurredAt: string;
}
interface CompanyDetail extends Omit<CompanySummary, 'updatedAt'> {
  credentials: Array<{
    moduleKey: CompanyFormInput['credentials'][number]['module'];
    label: string;
    username: string;
    password: string;
  }>;
}

async function request<T>(path: string, options: RequestInit = {}, token?: string): Promise<T> {
  const response = await fetch(`${apiUrl}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });
  if (!response.ok)
    throw new Error(
      response.status === 401
        ? 'Sua sessão expirou. Faça login novamente.'
        : 'Não foi possível concluir a operação.',
    );
  return response.status === 204 ? (undefined as T) : (response.json() as Promise<T>);
}

export const api = {
  login: (email: string, password: string) =>
    request<Session>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),
  register: (organizationName: string, name: string, email: string, password: string) =>
    request<void>('/api/auth/register-organization', {
      method: 'POST',
      body: JSON.stringify({ organizationName, name, email, password }),
    }),
  searchCompanies: (token: string, search = '') =>
    request<CompanySummary[]>(`/api/companies?search=${encodeURIComponent(search)}`, {}, token),
  createCompany: (token: string, company: CompanyFormInput) =>
    request<CompanySummary>(
      '/api/companies',
      { method: 'POST', body: JSON.stringify(company) },
      token,
    ),
  getCompany: async (token: string, id: string): Promise<CompanyFormInput> => {
    const company = await request<CompanyDetail>(`/api/companies/${id}`, {}, token);
    return {
      companyName: company.companyName,
      cnpj: company.cnpj,
      stateRegistration: company.stateRegistration,
      credentials: company.credentials.map(({ moduleKey, ...credential }) => ({
        module: moduleKey,
        ...credential,
      })),
    };
  },
  updateCompany: (token: string, id: string, company: CompanyFormInput) =>
    request<CompanySummary>(
      `/api/companies/${id}`,
      { method: 'PUT', body: JSON.stringify(company) },
      token,
    ),
  deleteCompany: (token: string, id: string) =>
    request<void>(`/api/companies/${id}`, { method: 'DELETE' }, token),
  listUsers: (token: string) => request<ManagedUser[]>('/api/users', {}, token),
  createUser: (token: string, user: Omit<ManagedUser, 'id' | 'isActive'> & { password: string }) =>
    request<ManagedUser>('/api/users', { method: 'POST', body: JSON.stringify(user) }, token),
  setUserActive: (token: string, id: string, isActive: boolean) =>
    request<void>(
      `/api/users/${id}/active`,
      { method: 'PATCH', body: JSON.stringify(isActive) },
      token,
    ),
  listAuditEvents: (token: string) => request<AuditEvent[]>('/api/audit-events', {}, token),
};
