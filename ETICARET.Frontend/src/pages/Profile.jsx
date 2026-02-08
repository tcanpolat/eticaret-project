import React, { useState, useEffect } from 'react';
import accountService from '../services/accountService';
import './Profile.css';

function Profile() {
  const [profile, setProfile] = useState({
    fullName: '',
    email: '',
    userName: '',
    phoneNumber: ''
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      const response = await accountService.getProfile();
      if (response.success) {
        setProfile({
          fullName: response.data.fullName,
          email: response.data.email,
          userName: response.data.userName,
          phoneNumber: response.data.phoneNumber || ''
        });
      } else {
        setError(response.message);
      }
    } catch (err) {
      setError('Profil bilgileri yüklenirken bir hata oluştu.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setProfile(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    try {
      const updateRequest = {
        fullName: profile.fullName,
        phoneNumber: profile.phoneNumber || null
      };
      
      const response = await accountService.updateProfile(updateRequest);
      
      if (response.success) {
        setSuccess('Profil başarıyla güncellendi.');
      } else {
        setError(response.message);
      }
    } catch (err) {
      console.error(err);
      if (err.response && err.response.data) {
        if (err.response.data.errors) {
          // ASP.NET Core varsayılan validasyon hataları
          const errorMessages = Object.values(err.response.data.errors).flat();
          setError(errorMessages.join('\n'));
        } else if (err.response.data.message) {
          // Custom ApiResponse hatası
          let msg = err.response.data.message;
          if (err.response.data.Errors && Array.isArray(err.response.data.Errors) && err.response.data.Errors.length > 0) {
             msg += ': ' + err.response.data.Errors.join(', ');
          }
          setError(msg);
        } else {
          setError('Profil güncellenirken bir hata oluştu.');
        }
      } else {
        setError('Profil güncellenirken bir hata oluştu.');
      }
    }
  };

  if (loading) return <div className="profile-container">Yükleniyor...</div>;

  return (
    <div className="profile-container">
      <div className="profile-card">
        <h2>Profil Bilgileri</h2>
        
        {error && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Kullanıcı Adı</label>
            <input 
              type="text" 
              value={profile.userName} 
              disabled 
              className="form-control disabled"
            />
            <small>Kullanıcı adı değiştirilemez.</small>
          </div>

          <div className="form-group">
            <label>Email</label>
            <input 
              type="email" 
              value={profile.email} 
              disabled 
              className="form-control disabled"
            />
            <small>Email adresi değiştirilemez.</small>
          </div>

          <div className="form-group">
            <label>Ad Soyad</label>
            <input 
              type="text" 
              name="fullName"
              value={profile.fullName} 
              onChange={handleChange}
              required
              className="form-control"
            />
          </div>

          <div className="form-group">
            <label>Telefon Numarası</label>
            <input 
              type="tel" 
              name="phoneNumber"
              value={profile.phoneNumber} 
              onChange={handleChange}
              className="form-control"
              placeholder="05xxxxxxxxx"
            />
          </div>

          <button type="submit" className="btn-save">Kaydet</button>
        </form>
      </div>
    </div>
  );
}

export default Profile;
