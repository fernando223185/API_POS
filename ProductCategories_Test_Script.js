// ====================================
// SCRIPT DE PRUEBA - CATEGORÍAS DE PRODUCTOS
// ====================================

// 1. Test de obtener todas las categorías
console.log("?? Testing Product Categories API...");

// Primero necesitas hacer login para obtener el token
const loginTest = {
  url: 'http://192.168.192.57:7254/api/login/login',
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    code: 'ADMIN001',
    password: 'admin123'
  })
};

console.log("1. Login test:", loginTest);

// 2. Test de obtener categorías (después del login)
const getCategoriesTest = {
  url: 'http://192.168.192.57:7254/api/productcategories',
  method: 'GET',
  headers: {
    'Authorization': 'Bearer [TOKEN_AQUI]',
    'Content-Type': 'application/json'
  }
};

console.log("2. Get categories test:", getCategoriesTest);

// 3. Test de dropdown
const getDropdownTest = {
  url: 'http://192.168.192.57:7254/api/productcategories/dropdown',
  method: 'GET',
  headers: {
    'Authorization': 'Bearer [TOKEN_AQUI]',
    'Content-Type': 'application/json'
  }
};

console.log("3. Get dropdown test:", getDropdownTest);

// 4. Test de crear producto con categoría
const createProductWithCategoryTest = {
  url: 'http://192.168.192.57:7254/api/products/basic',
  method: 'POST',
  headers: {
    'Authorization': 'Bearer [TOKEN_AQUI]',
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    name: "iPhone 15 Pro Max",
    description: "Smartphone premium Apple",
    code: "IPH15PM-256",
    price: 28999.99,
    baseCost: 18500.00,
    brand: "Apple",
    minimumStock: 5,
    maximumStock: 50,
    categoryId: 1  // Electrónica
  })
};

console.log("4. Create product with category test:", createProductWithCategoryTest);

// 5. Test de estadísticas
const getStatsTest = {
  url: 'http://192.168.192.57:7254/api/productcategories/stats',
  method: 'GET',
  headers: {
    'Authorization': 'Bearer [TOKEN_AQUI]',
    'Content-Type': 'application/json'
  }
};

console.log("5. Get stats test:", getStatsTest);

console.log("? All test configurations ready!");
console.log("?? Replace [TOKEN_AQUI] with actual JWT token from login response");

// ====================================
// CURL COMMANDS (para terminal)
// ====================================

const curlCommands = `
# 1. Login
curl -X POST http://192.168.192.57:7254/api/login/login \\
  -H "Content-Type: application/json" \\
  -d '{"code":"ADMIN001","password":"admin123"}' \\
  -v

# 2. Get Categories (replace YOUR_TOKEN)
curl -X GET http://192.168.192.57:7254/api/productcategories \\
  -H "Authorization: Bearer YOUR_TOKEN" \\
  -H "Content-Type: application/json" \\
  -v

# 3. Get Categories for Dropdown
curl -X GET http://192.168.192.57:7254/api/productcategories/dropdown \\
  -H "Authorization: Bearer YOUR_TOKEN" \\
  -H "Content-Type: application/json" \\
  -v

# 4. Get Category by ID (Electrónica)
curl -X GET http://192.168.192.57:7254/api/productcategories/1 \\
  -H "Authorization: Bearer YOUR_TOKEN" \\
  -H "Content-Type: application/json" \\
  -v

# 5. Create Product with Category
curl -X POST http://192.168.192.57:7254/api/products/basic \\
  -H "Authorization: Bearer YOUR_TOKEN" \\
  -H "Content-Type: application/json" \\
  -d '{
    "name": "iPhone 15 Pro Max",
    "description": "Smartphone premium Apple",
    "code": "IPH15PM-001",
    "price": 28999.99,
    "baseCost": 18500.00,
    "brand": "Apple",
    "minimumStock": 5,
    "maximumStock": 50
  }' \\
  -v
`;

console.log("?? CURL Commands:");
console.log(curlCommands);