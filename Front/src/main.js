import { HomePage } from './pages/home/index.js';

const app = document.querySelector('#app');

if (app) {
  app.append(HomePage());
}
