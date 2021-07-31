const express = require('express')
const bodyParser = require('body-parser')

const app = express()
app.use(bodyParser.json({ type: 'application/*+json' }));

app.get('/dapr/subscribe', (req, res) => {
    res.json([
        {
            pubsubname: "sensors",
            topic: "temperature",
            route: "temperature-measurement"
        }
    ]);
})

app.post('/temperature-measurement', (req, res) => {
    const temperature = req.body.data.temperatureInCelsius;
    let message = '';
    if (temperature > 31) {
        message = "It's scorching!";
    } else if (temperature > 24) {
        message = "It's hot!";
    } else if (temperature > 17) {
        message = "It's warm!";
    } else if (temperature > 8) {
        message = "It's cool!";
    } else {
        message = "It's cold!";
    }

    let date = new Date(req.body.data.eventTime);
    console.log(`The temperature is ${temperature} degrees Celsius at ${date.toLocaleString()}. ${message}`);
    res.sendStatus(200);
});

const port = 8000;
app.listen(port,
    () => console.log(`Listening on port ${port}`));