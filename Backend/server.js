const express = require("express");
const http = require('http');
const cors = require("cors");

const HttpError = require("./models/http-error");
const errorHandler = require("./error-handler");
const mongo = require("./mongoConnection");
const usersRouter = require("./routes/users-routes");
const systemRouter = require("./routes/system-routes");

const app = express();
const server = http.createServer(app);

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.use(cors());

// Routes
app.use("/api/system", systemRouter);
app.use("/api/users", usersRouter);


// Not found
app.use((req, res, next) => {
    const error = new HttpError("Not found!", 404);
    throw error;
});

// Errors
app.use(errorHandler);

server.listen(7007, () => {
    // Connect ot mongoDb   
    mongo.connect();

    console.log('Server is running');
});