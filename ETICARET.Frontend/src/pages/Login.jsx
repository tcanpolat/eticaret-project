import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import authService from '../services/authService';
import './Auth.css';

function Login() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });
  const [error, setError] = useState('');

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prevState => ({
      ...prevState,
      [name]: value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(''); // Hataları temizle

    try {
      const response = await authService.login(formData);
      
      // API'den dönen yapıyı görelim
      console.log("Login response (JSON):", JSON.stringify(response, null, 2));
      console.log("Login response keys:", typeof response === 'object' ? Object.keys(response) : 'string');

      // API'den dönen response bir obje, tüm olası anahtarları deniyoruz
      let token = null;

      if (typeof response === 'string') {
        token = response;
      } else if (typeof response === 'object' && response !== null) {
        // Obje içindeki tüm anahtarları dolaşıp JWT benzeri bir string buluyoruz
        token = response.token 
          || response.accessToken 
          || response.Token 
          || response.AccessToken
          || response.data?.token
          || response.data?.Token
          || response.result
          || response.Result;
        
        // Hala bulamadıysak, obje içindeki ilk string değeri JWT olabilir mi kontrol et
        if (!token) {
          for (const key of Object.keys(response)) {
            const val = response[key];
            if (typeof val === 'string' && val.startsWith('eyJ')) {
              token = val;
              console.log(`Token '${key}' anahtarında bulundu.`);
              break;
            }
          }
        }
      }

      if (token) {
        localStorage.setItem('token', token);
        console.log("Token kaydedildi (ilk 50):", token.substring(0, 50));
        navigate('/');
      } else {
        console.error("Token bulunamadı! Response:", JSON.stringify(response));
        setError('Token alınamadı. Console loglarını kontrol edin.');
      }
    } catch (err) {
      console.error("Login Error:", err);
      // Hata mesajını kullanıcıya göster
      setError(err.response?.data?.message || 'Giriş yapılırken bir hata oluştu. Lütfen bilgilerinizi kontrol edin.');
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2 className="auth-title">Login</h2>
        {error && <div className="error-message" style={{ color: 'red', marginBottom: '10px' }}>{error}</div>}
        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input 
              type="email" 
              name="email" 
              placeholder="Enter your email" 
              value={formData.email}
              onChange={handleChange}
              required
            />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input 
              type="password" 
              name="password" 
              placeholder="Enter your password" 
              value={formData.password}
              onChange={handleChange}
              required
            />
          </div>
          <button type="submit" className="auth-button">Login</button>
        </form>
      </div>
    </div>
  );
}

export default Login;
