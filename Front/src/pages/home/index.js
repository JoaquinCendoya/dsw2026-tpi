import { AppShell } from '../../shared/components/index.js';

export function HomePage() {
  const content = document.createElement('section');
  content.className = 'page-placeholder';
  content.innerHTML = `
    <p class="page-placeholder__eyebrow">Frontend</p>
    <h1>Gestión de turnos médicos</h1>
    <p>Arquitectura inicial preparada para comenzar el desarrollo por módulos.</p>
  `;

  return AppShell(content);
}
