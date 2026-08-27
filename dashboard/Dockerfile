FROM node:24-alpine AS build
WORKDIR /app
COPY package*.json ./
# npm install yerine npm ci: lockfile'a birebir sadik, tekrarlanabilir kurulum
RUN npm ci --ignore-scripts
COPY . .

# CRA ortam degiskenlerini BUILD sirasinda bundle'a gomer; compose'daki
# "environment:" altinda vermenin hicbir etkisi yoktu.
ARG REACT_APP_API_BASE=""
ENV REACT_APP_API_BASE=$REACT_APP_API_BASE
RUN npm run build

# nginx:alpine root olarak calisiyordu. Unprivileged varyant uid 101 ile
# calisir ve 8080 dinler (1024 altindaki portlar root ister).
FROM nginxinc/nginx-unprivileged:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]