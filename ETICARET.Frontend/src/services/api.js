import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7252/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: Token eklemek için
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor: Hata yönetimi için (Opsiyonel)
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // Örnek: 401 hatası alınırsa kullanıcıyı logout yapabiliriz
    if (error.response && error.response.status === 401) {
      // localStorage.removeItem('token');
      // window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
