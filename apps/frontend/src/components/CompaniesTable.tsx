import { Edit3, Trash2 } from 'lucide-react';
import type { CompanySummary } from '../services/api';
import { formatCnpj } from '../utils/cnpj';

interface CompaniesTableProps {
  companies: CompanySummary[];
  onEdit: (company: CompanySummary) => void;
  onDelete: (company: CompanySummary) => void;
}
export function CompaniesTable({ companies, onEdit, onDelete }: CompaniesTableProps) {
  return (
    <section className="content-card records-card">
      <div className="card-title">
        <div>
          <h2>Empresas cadastradas</h2>
          <p>{companies.length} resultado(s) encontrado(s).</p>
        </div>
      </div>
      <div className="records-table">
        <div className="records-head">
          <span>Razão social</span>
          <span>CNPJ</span>
          <span>IE</span>
          <span>Ações</span>
        </div>
        {companies.map((company) => (
          <div className="records-row" key={company.id}>
            <strong>{company.companyName}</strong>
            <span>{formatCnpj(company.cnpj)}</span>
            <span>{company.stateRegistration || '—'}</span>
            <span>
              <button
                className="icon-button"
                onClick={() => onEdit(company)}
                aria-label={`Editar ${company.companyName}`}
              >
                <Edit3 size={17} />
              </button>
              <button
                className="icon-button danger"
                onClick={() => onDelete(company)}
                aria-label={`Excluir ${company.companyName}`}
              >
                <Trash2 size={17} />
              </button>
            </span>
          </div>
        ))}
        {companies.length === 0 && <p className="empty-state">Nenhuma empresa encontrada.</p>}
      </div>
    </section>
  );
}
