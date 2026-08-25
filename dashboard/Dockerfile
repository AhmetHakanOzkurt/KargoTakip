FROM node:24-alpine AS build
WORKDIR /app
COPY package*.json ./
# npm install yerine npm ci: lockfile'a birebir sadik, tekrarlanabilir kurulum
RUN npm ci
COPY . .

# CRA ortam degiskenlerini BUILD sirasinda bundle'a gomer; compose'daki
# "environment:" altinda vermenin hicbir etkisi yoktu.
ARG REACT_APP_API_BASE=""
ENV REACT_APP_API_BASE=$REACT_APP_API_BASE
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]