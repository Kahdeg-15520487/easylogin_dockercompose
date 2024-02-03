FROM node:20

RUN mkdir /app
COPY app.js package.json yarn.lock /app/
WORKDIR /app
RUN npm i
ENV FLAG="W1{c7bc1bba-39c6-4936-b1c6-ad4519d9dc7c}"
RUN chmod +x /app/app.js

# Check if Express.js module exists
RUN result=$(node -e "require('express')") && echo $result

EXPOSE 3000
ENTRYPOINT [ "/app/app.js" ]