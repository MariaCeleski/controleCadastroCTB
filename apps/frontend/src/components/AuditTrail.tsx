import { ClipboardList, RefreshCw } from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import { api, type AuditEvent } from '../services/api';

const actionLabels: Record<AuditEvent['action'], string> = {
  OrganizationRegistered: 'Organização cadastrada',
  UserAuthenticated: 'Login realizado',
  CompanyCreated: 'Empresa criada',
  CompanyCredentialsViewed: 'Credenciais consultadas',
  CompanyUpdated: 'Empresa atualizada',
  CompanyDeleted: 'Empresa excluída',
  UserCreated: 'Usuário criado',
  UserActivationChanged: 'Status de usuário alterado',
};

export function AuditTrail({ token }: { token: string }) {
  const [events, setEvents] = useState<AuditEvent[]>([]);
  const [error, setError] = useState<string>();
  const load = useCallback(async () => {
    try {
      setError(undefined);
      setEvents(await api.listAuditEvents(token));
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível carregar a auditoria.');
    }
  }, [token]);
  useEffect(() => {
    void load();
  }, [load]);
  return (
    <section className="content-card audit-card" id="audit">
      <div className="card-title audit-title">
        <div className="title-icon">
          <ClipboardList size={19} />
        </div>
        <div>
          <h2>Auditoria</h2>
          <p>Últimas 100 ações sensíveis registradas para esta organização.</p>
        </div>
        <button
          className="icon-button"
          type="button"
          aria-label="Atualizar auditoria"
          onClick={() => void load()}
        >
          <RefreshCw size={18} />
        </button>
      </div>
      {error && <p className="form-error">{error}</p>}
      {!error && events.length === 0 && (
        <p className="empty-state">Nenhum evento registrado ainda.</p>
      )}
      <div className="audit-list">
        {events.map((event) => (
          <div className="audit-row" key={event.id}>
            <span className="audit-marker" aria-hidden="true" />
            <div>
              <strong>{actionLabels[event.action]}</strong>
              <p>{event.entityType}</p>
            </div>
            <time dateTime={event.occurredAt}>
              {new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(
                new Date(event.occurredAt),
              )}
            </time>
          </div>
        ))}
      </div>
    </section>
  );
}
