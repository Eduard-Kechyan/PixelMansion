var express = require('express');

var app = express();

const PORT = process.env.PORT || 5050

app.get('/', (req, res) => {
    res.send('Hi!')
})

app.listen(PORT, function () {
    console.log(`Listening at: ${PORT}!`);
});