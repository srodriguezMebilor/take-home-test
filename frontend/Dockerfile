# Etapa 1: Compilación
# Angular 19 funciona mejor con Node 20 o 22. Usamos la imagen oficial de Node.
FROM node:22-alpine as build
WORKDIR /app

# Copiamos package.json y package-lock.json (si existe)
COPY package*.json ./

# Usamos 'npm ci' en lugar de install para entornos de CI/CD (más limpio y rápido)
RUN if [ -f package-lock.json ]; then npm ci; else npm install; fi

# Copiamos el código fuente
COPY . .

# Compilamos la aplicación.
# Angular 19 por defecto compila en modo producción al correr build.
RUN npm run build

# Etapa 2: Servidor Nginx
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Limpiamos archivos default de Nginx
RUN rm -rf ./*

# --- AQUÍ ESTÁ EL CAMBIO CLAVE ---
# En Angular 19 el output suele ser /dist/frontend/browser
# Si falla, cambia la ruta de abajo a: /app/dist/frontend
COPY --from=build /app/dist/temp/browser .

# Copiamos la configuración de nginx (asegúrate de haber creado el archivo nginx.conf)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
