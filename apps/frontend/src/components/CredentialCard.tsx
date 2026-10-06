import { Copy, Eye, EyeOff, KeyRound } from 'lucide-react';
import { useState } from 'react';
import type { CredentialInput } from '../types/company';

interface CredentialCardProps {
  credential: CredentialInput;
  onChange: (next: CredentialInput) => void;
}

export function CredentialCard({ credential, onChange }: CredentialCardProps) {
  const [visible, setVisible] = useState(false);
  const update = (field: keyof CredentialInput, value: string) =>
    onChange({ ...credential, [field]: value });
  const copyUsername = async () => {
    if (credential.username) await navigator.clipboard.writeText(credential.username);
  };
  return (
    <section className="credential-card">
      <header>
        <span>
          <KeyRound size={18} />
          {credential.label}
        </span>
      </header>
      <label>
        Usuário
        <input
          value={credential.username}
          onChange={(event) => update('username', event.target.value)}
          autoComplete="off"
        />
      </label>
      <label>
        Senha
        <span className="password-field">
          <input
            type={visible ? 'text' : 'password'}
            value={credential.password}
            onChange={(event) => update('password', event.target.value)}
            autoComplete="new-password"
          />
          <button
            type="button"
            className="icon-button"
            onClick={() => setVisible(!visible)}
            aria-label={visible ? 'Ocultar senha' : 'Exibir senha'}
          >
            {visible ? <EyeOff size={17} /> : <Eye size={17} />}
          </button>
        </span>
      </label>
      <button type="button" className="copy-button" onClick={copyUsername}>
        <Copy size={15} /> Copiar usuário
      </button>
    </section>
  );
}
