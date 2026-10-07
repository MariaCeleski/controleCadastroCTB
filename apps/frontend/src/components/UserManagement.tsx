import { useCallback, useEffect, useState } from 'react';
import { api, type ManagedUser } from '../services/api';

export function UserManagement({ token }: { token: string }) {
  const [users, setUsers] = useState<ManagedUser[]>([]);
  const [error, setError] = useState<string>();
  const load = useCallback(async () => setUsers(await api.listUsers(token)), [token]);
  useEffect(() => {
    void load();
  }, [load]);
  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    try {
      await api.createUser(token, {
        name: String(data.get('name')),
        email: String(data.get('email')),
        password: String(data.get('password')),
        role: String(data.get('role')) as ManagedUser['role'],
      });
      event.currentTarget.reset();
      await load();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível criar o usuário.');
    }
  };
  return (
    <section className="content-card records-card" id="users">
      <div className="card-title">
        <div>
          <h2>Usuários</h2>
          <p>Contas internas desta organização.</p>
        </div>
      </div>
      <form className="fields-grid users-form" onSubmit={submit}>
        <label>
          Nome
          <input name="name" required minLength={3} />
        </label>
        <label>
          E-mail
          <input name="email" type="email" required />
        </label>
        <label>
          Senha inicial
          <input name="password" type="password" required minLength={12} />
        </label>
        <label>
          Perfil
          <select name="role" defaultValue="Operator">
            <option value="Operator">Operador</option>
            <option value="Administrator">Administrador</option>
          </select>
        </label>
        <button className="primary" type="submit">
          Adicionar
        </button>
      </form>
      {error && <p className="form-error">{error}</p>}
      <div className="records-table users-records">
        <div className="records-head">
          <span>Nome</span>
          <span>E-mail</span>
          <span>Perfil</span>
          <span>Ações</span>
        </div>
        {users.map((user) => (
          <div className="records-row" key={user.id}>
            <strong>{user.name}</strong>
            <span>{user.email}</span>
            <span>{user.role}</span>
            <span>
              <button
                className="secondary"
                type="button"
                onClick={async () => {
                  await api.setUserActive(token, user.id, !user.isActive);
                  await load();
                }}
              >
                {user.isActive ? 'Bloquear' : 'Ativar'}
              </button>
            </span>
          </div>
        ))}
      </div>
    </section>
  );
}
