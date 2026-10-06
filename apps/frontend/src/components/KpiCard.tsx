import type { ReactNode } from 'react';

interface KpiCardProps {
  label: string;
  value: string;
  icon: ReactNode;
  trend: string;
}

export function KpiCard({ label, value, icon, trend }: KpiCardProps) {
  return (
    <article className="kpi-card">
      <span className="kpi-icon">{icon}</span>
      <div>
        <p>{label}</p>
        <strong>{value}</strong>
        <small>{trend}</small>
      </div>
    </article>
  );
}
