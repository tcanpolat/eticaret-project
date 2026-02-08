import api from './api';

const accountService = {
  getProfile: async () => {
    const response = await api.get('/Account/profile');
    return response.data;
  },
  updateProfile: async (updateProfileRequest) => {
    // updateProfileRequest: { fullName, phoneNumber }
    const response = await api.put('/Account/update', updateProfileRequest);
    return response.data;
  }
};

export default accountService;
