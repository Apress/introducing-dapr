const express = require('express');
const fetch = require('node-fetch');

const app = express();
const daprPort = process.env.DAPR_HTTP_PORT;
const invokeHello = `http://localhost:${daprPort}/v1.0/invoke/hello-service/method/sayHello`;
const invokeWorld = `http://localhost:${daprPort}/v1.0/invoke/world-service/method/sayWorld`;

app.get('/greet', async (_, res) => {
    hello = await fetch(invokeHello);
    world = await fetch(invokeWorld);
    const greeting = await hello.text() + ' ' + await world.text();
    
    console.log(`Sending: ${greeting}`);
    res.send(greeting);
});

const port = 8090;
app.listen(port,
    () => console.log(`Listening on port ${port}`));