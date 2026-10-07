export type CredentialModule = 'erp' | 'fiscal' | 'financial' | 'administrative';

export interface CredentialInput {
  module: CredentialModule;
  label: string;
  username: string;
  password: string;
}

export interface CompanyFormInput {
  companyName: string;
  registrationNumber: string;
  cnpj: string;
  stateRegistration?: string;
  credentials: CredentialInput[];
}
