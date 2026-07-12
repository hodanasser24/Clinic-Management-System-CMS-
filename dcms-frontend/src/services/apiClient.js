import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7299',
  headers: { 'Content-Type': 'application/json' },
});

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  
  failedQueue = [];
};

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    if (error.response?.status === 401 && !original._retry) {
      if (isRefreshing) {
        return new Promise(function(resolve, reject) {
          failedQueue.push({ resolve, reject });
        }).then(token => {
          original.headers.Authorization = `Bearer ${token}`;
          return apiClient(original);
        }).catch(err => {
          return Promise.reject(err);
        });
      }

      original._retry = true;
      isRefreshing = true;

      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        try {
          const { data } = await axios.post(
            `${apiClient.defaults.baseURL}/api/Auth/refresh`,
            { accessToken: localStorage.getItem('token'), refreshToken },
          );
          
          localStorage.setItem('token', data.accessToken);
          localStorage.setItem('refreshToken', data.refreshToken);
          
          original.headers.Authorization = `Bearer ${data.accessToken}`;
          processQueue(null, data.accessToken);
          
          return apiClient(original);
        } catch (err) {
          processQueue(err, null);
          
          // Complete cleanup on failed refresh
          localStorage.removeItem('token');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('userId');
          localStorage.removeItem('userName');
          localStorage.removeItem('email');
          localStorage.removeItem('role');
          
          window.location.href = '/login';
        } finally {
          isRefreshing = false;
        }
      } else {
        isRefreshing = false;
      }
    }
    return Promise.reject(error);
  },
);

export default apiClient;
