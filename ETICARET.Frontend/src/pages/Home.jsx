import React, { useEffect, useState } from 'react'
import productService from '../services/productService'
import './Home.css'

function Home() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // HTML tag'lerini temizleyen yardımcı fonksiyon
  const stripHtmlTags = (html) => {
    if (!html) return '';
    return html.replace(/<[^>]*>/g, '').trim();
  };

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const data = await productService.getAll();
        
        let productList = [];
        if (Array.isArray(data)) {
          productList = data;
        } else if (data?.$values) {
          productList = data.$values;
        } else if (data?.data) {
          productList = Array.isArray(data.data) ? data.data : [data.data];
        } else if (data?.result) {
          productList = Array.isArray(data.result) ? data.result : [data.result];
        } else if (data?.products) {
          productList = data.products;
        } else if (data?.items) {
          productList = data.items;
        }

        setProducts(productList);
        setLoading(false);
      } catch (err) {
        const errorMsg = err.response?.status === 401 
          ? "Yetkilendirme hatası. Lütfen tekrar giriş yapın." 
          : "Ürünler yüklenirken hata oluştu.";
        setError(errorMsg);
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

  if (loading) {
    return <div className="home-container" style={{textAlign: 'center', marginTop: '50px'}}>Yükleniyor...</div>;
  }

  if (error) {
    return <div className="home-container" style={{textAlign: 'center', marginTop: '50px', color: 'red'}}>{error}</div>;
  }

  return (
    <div className="home-container">
      <div className="hero-section">
        <h1 className="hero-title">Welcome to E-Ticaret</h1>
        <p className="hero-subtitle">Discover our premium collection of products designed for your lifestyle.</p>
      </div>
      
      <div className="products-grid">
        {products.map(product => (
          <div key={product.id} className="product-card">
            {/* API'den gelen resim url'i yoksa placeholder kullan */}
            <img 
              src={product.image || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&q=80'} 
              alt={product.name} 
              className="product-image" 
            />
            <div className="product-info">
              <h3 className="product-title">{product.name}</h3>
              <p className="product-description">{stripHtmlTags(product.description) || 'Ürün açıklaması bulunamadı.'}</p>
              <div className="product-footer">
                <span className="product-price">${product.price}</span>
                <button className="add-to-cart-btn">Add to Cart</button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

export default Home
