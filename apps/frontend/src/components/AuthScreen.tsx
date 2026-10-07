import { Eye, EyeOff, ShieldCheck } from 'lucide-react';
import { useState } from 'react';

interface AuthScreenProps {
  onLogin: (email: string, password: string) => Promise<void>;
  onRegister: (
    organization: string,
    name: string,
    email: string,
    password: string,
  ) => Promise<void>;
}
export function AuthScreen({ onLogin, onRegister }: AuthScreenProps) {
  const [registering, setRegistering] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>();
  const [passwordVisible, setPasswordVisible] = useState(false);
  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setLoading(true);
    setError(undefined);
    try {
      if (registering)
        await onRegister(
          String(data.get('organization')),
          String(data.get('name')),
          String(data.get('email')),
          String(data.get('password')),
        );
      else await onLogin(String(data.get('email')), String(data.get('password')));
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Não foi possível autenticar.');
    } finally {
      setLoading(false);
    }
  };
  return (
    <main className="auth-page">
      <form className="auth-card" onSubmit={submit}>
        <span className="auth-logo">
          <ShieldCheck />
        </span>
        <h1>{registering ? 'Criar organização' : 'Acessar plataforma'}</h1>
        <p>
          {registering
            ? 'Crie o primeiro administrador da sua organização.'
            : 'Use suas credenciais corporativas para continuar.'}
        </p>
        {registering && (
          <>
            <label>
              Organização
              <input name="organization" required minLength={3} />
            </label>
            <label>
              Seu nome
              <input name="name" required minLength={3} />
            </label>
          </>
        )}
        <label>
          E-mail
          <input name="email" type="email" autoComplete="email" required />
        </label>
        <label>
          Senha
          <span className="auth-password-field">
            <input
              name="password"
              type={passwordVisible ? 'text' : 'password'}
              autoComplete={registering ? 'new-password' : 'current-password'}
              required
              minLength={12}
            />
            <button
              type="button"
              className="auth-password-toggle"
              aria-label={passwordVisible ? 'Ocultar senha' : 'Exibir senha'}
              aria-pressed={passwordVisible}
              onClick={() => setPasswordVisible(!passwordVisible)}
            >
              {passwordVisible ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </span>
        </label>
        {error && <p className="form-error">{error}</p>}
        <button className="primary" disabled={loading}>
          {loading ? 'Aguarde...' : registering ? 'Criar e acessar' : 'Entrar'}
        </button>
        <button type="button" className="auth-link" onClick={() => setRegistering(!registering)}>
          {registering ? 'Já possui acesso? Entrar' : 'Criar a primeira organização'}
        </button>
      </form>
    </main>
  );
}
