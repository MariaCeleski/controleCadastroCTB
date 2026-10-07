import { Building2, Save, X } from 'lucide-react';
import { useEffect, useState } from 'react';
import { CredentialCard } from './CredentialCard';
import type { CompanyFormInput, CredentialModule } from '../types/company';
import { formatCnpj, isValidCnpj } from '../utils/cnpj';

const modules: Array<[CredentialModule, string]> = [
  ['erp', 'Sistema principal / ERP'],
  ['fiscal', 'Portal fiscal / SEFAZ'],
  ['financial', 'Internet banking / Financeiro'],
  ['administrative', 'Painel administrativo'],
];
const initialForm = (): CompanyFormInput => ({
  companyName: '',
  registrationNumber: '',
  cnpj: '',
  stateRegistration: '',
  credentials: modules.map(([module, label]) => ({ module, label, username: '', password: '' })),
});

interface CompanyFormProps {
  value?: CompanyFormInput;
  onSave: (form: CompanyFormInput) => Promise<void>;
}
export function CompanyForm({ value, onSave }: CompanyFormProps) {
  const [form, setForm] = useState<CompanyFormInput>(initialForm);
  const [error, setError] = useState<string>();
  useEffect(() => {
    setForm(value ?? initialForm());
    setError(undefined);
  }, [value]);
  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (form.companyName.trim().length < 3)
      return setError('Informe uma razão social com pelo menos 3 caracteres.');
    if (!/^\d{1,20}$/.test(form.registrationNumber))
      return setError('Informe um número de cadastro com até 20 dígitos.');
    if (!isValidCnpj(form.cnpj)) return setError('Informe um CNPJ válido.');
    try {
      await onSave(form);
      setForm(initialForm());
      setError(undefined);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível salvar a empresa.');
    }
  };
  return (
    <form className="content-card" onSubmit={submit}>
      <div className="card-title">
        <span className="title-icon">
          <Building2 size={20} />
        </span>
        <div>
          <h2>{value ? 'Editar empresa' : 'Nova empresa'}</h2>
          <p>Cadastre os dados e as credenciais de acesso.</p>
        </div>
      </div>
      <div className="fields-grid">
        <label>
          Número de cadastro
          <input
            value={form.registrationNumber}
            onChange={(event) =>
              setForm({
                ...form,
                registrationNumber: event.target.value.replace(/\D/g, ''),
              })
            }
            placeholder="Ex.: 000123"
            inputMode="numeric"
          />
        </label>
        <label>
          Razão social
          <input
            value={form.companyName}
            onChange={(event) => setForm({ ...form, companyName: event.target.value })}
            placeholder="Ex.: Empresa Exemplo Ltda."
          />
        </label>
        <label>
          CNPJ
          <input
            value={form.cnpj}
            onChange={(event) => setForm({ ...form, cnpj: formatCnpj(event.target.value) })}
            placeholder="00.000.000/0000-00"
            inputMode="numeric"
          />
        </label>
        <label>
          Inscrição estadual <small>(opcional)</small>
          <input
            value={form.stateRegistration}
            onChange={(event) => setForm({ ...form, stateRegistration: event.target.value })}
            placeholder="Digite ou informe isento"
          />
        </label>
      </div>
      {error && (
        <p className="form-error" role="alert">
          {error}
        </p>
      )}
      <div className="credentials-grid">
        {form.credentials.map((credential, index) => (
          <CredentialCard
            key={credential.module}
            credential={credential}
            onChange={(next) =>
              setForm({
                ...form,
                credentials: form.credentials.map((item, itemIndex) =>
                  itemIndex === index ? next : item,
                ),
              })
            }
          />
        ))}
      </div>
      <footer className="form-actions">
        <button
          type="button"
          className="secondary"
          onClick={() => {
            setForm(initialForm());
            setError(undefined);
          }}
        >
          <X size={17} /> Limpar
        </button>
        <button type="submit" className="primary">
          <Save size={17} /> Salvar empresa
        </button>
      </footer>
    </form>
  );
}
