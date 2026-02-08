import api from './api';

const authService = {
  login: async (loginRequest) => {
    // loginRequest: { email, password }
    const response = await api.post('/Login/login', loginRequest);
    return response.data;
  },
  register: async (registerRequest) => {
    // registerRequest: { fullName, userName, email, password, confirmPassword }
    const response = await api.post('/Login/register', registerRequest);
    return response.data;
  }
};

export default authService;
