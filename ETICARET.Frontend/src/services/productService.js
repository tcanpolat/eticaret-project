import api from './api';

const productService = {
  getAll: async () => {
    const response = await api.get('/Product/getall');
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(`/Product/${id}`);
    return response.data;
  }
};

export default productService;
