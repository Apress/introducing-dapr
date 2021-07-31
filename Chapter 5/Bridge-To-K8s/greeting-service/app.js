const express = require('express');
const fetch = require('node-fetch');

const app = express();
const daprHost= process.env.DAPR_SIDECAR_HOST;
const invokeHello = `http://${daprHost}/v1.0/invoke/hello-service/method/sayHello`;
const invokeWorld = `http://${daprHost}/v1.0/invoke/world-service/method/sayWorld`;

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