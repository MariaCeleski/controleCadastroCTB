import {
  Bell,
  Building2,
  KeyRound,
  LayoutDashboard,
  Search,
  ShieldCheck,
  Users,
} from 'lucide-react';
import { useEffect, useState } from 'react';
import { AuthScreen } from './components/AuthScreen';
import { CompanyForm } from './components/CompanyForm';
import { CompaniesTable } from './components/CompaniesTable';
import { KpiCard } from './components/KpiCard';
import { api, type CompanySummary, type Session } from './services/api';
import type { CompanyFormInput } from './types/company';

export function App() {
  const [session, setSession] = useState<Session>();
  const [companies, setCompanies] = useState<CompanySummary[]>([]);
  const [search, setSearch] = useState('');
  const [editing, setEditing] = useState<{ id: string; value: CompanyFormInput }>();
  const loadCompanies = async (token: string, value = '') =>
    setCompanies(await api.searchCompanies(token, value));
  useEffect(() => {
    if (session) void loadCompanies(session.accessToken, search);
  }, [session, search]);
  if (!session)
    return (
      <AuthScreen
        onLogin={async (email, password) => setSession(await api.login(email, password))}
        onRegister={async (organization, name, email, password) => {
          await api.register(organization, name, email, password);
          setSession(await api.login(email, password));
        }}
      />
    );
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <span>
            <ShieldCheck size={22} />
          </span>
          <strong>
            Access<span>Control</span>
          </strong>
        </div>
        <nav>
          <a className="active" href="#dashboard">
            <LayoutDashboard size={19} /> Visão geral
          </a>
          <a href="#companies">
            <Building2 size={19} /> Empresas
          </a>
          <a href="#credentials">
            <KeyRound size={19} /> Credenciais
          </a>
          <a href="#users">
            <Users size={19} /> Usuários
          </a>
        </nav>
        <div className="sidebar-user">
          <span>MC</span>
          <div>
            <strong>{session.userName}</strong>
            <small>{session.role}</small>
          </div>
        </div>
      </aside>
      <main>
        <header className="topbar">
          <div>
            <p>Olá, {session.userName}</p>
            <h1>Controle de acessos</h1>
          </div>
          <div className="topbar-actions">
            <label className="search">
              <Search size={18} />
              <input
                placeholder="Buscar empresa ou CNPJ"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
              />
            </label>
            <button className="notification" aria-label="Notificações">
              <Bell size={19} />
              <i />
            </button>
          </div>
        </header>
        <section className="kpi-grid">
          <KpiCard
            label="Empresas cadastradas"
            value={String(companies.length)}
            trend="Resultados carregados"
            icon={<Building2 size={21} />}
          />
          <KpiCard
            label="Credenciais ativas"
            value={String(companies.length * 4)}
            trend="Módulos configuráveis"
            icon={<KeyRound size={21} />}
          />
          <KpiCard
            label="Acessos protegidos"
            value="100%"
            trend="Dados criptografados"
            icon={<ShieldCheck size={21} />}
          />
        </section>
        <CompanyForm
          value={editing?.value}
          onSave={async (company) => {
            if (editing) await api.updateCompany(session.accessToken, editing.id, company);
            else await api.createCompany(session.accessToken, company);
            setEditing(undefined);
            await loadCompanies(session.accessToken, search);
          }}
        />
        <CompaniesTable
          companies={companies}
          onEdit={async (company) =>
            setEditing({
              id: company.id,
              value: await api.getCompany(session.accessToken, company.id),
            })
          }
          onDelete={async (company) => {
            if (
              window.confirm(`Excluir ${company.companyName}? Esta ação não pode ser desfeita.`)
            ) {
              await api.deleteCompany(session.accessToken, company.id);
              await loadCompanies(session.accessToken, search);
            }
          }}
        />
      </main>
    </div>
  );
}
