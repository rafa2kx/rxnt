# Stage 1: Build Angular app
FROM node:18-alpine AS builder

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy source code
COPY . .

# Build Angular app
RUN npm run build

# Stage 2: Serve with nginx
FROM nginx:alpine

# Copy built app to nginx
COPY --from=builder /app/dist/rxnt-clinic-manager /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
