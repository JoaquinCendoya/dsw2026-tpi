export function AppShell(content) {
  const shell = document.createElement('main');
  shell.className = 'app-shell';
  shell.append(content);

  return shell;
}
