const mongoose = require("mongoose");

//const URI = 'mongodb+srv://Admin:Admin@grvenq.gtbynkr.mongodb.net/grvenq?retryWrites=true&w=majority';

const URI = "mongodb://127.0.0.1:27017/PixelMerge";

mongoose.set('strictQuery', true);

module.exports = {
    connect: () => {
        mongoose
            .connect(URI, { useNewUrlParser: true, useUnifiedTopology: true })
            .then(() => {
                console.log("Successfully connected to MongoDB.");
            })
            .catch((error) => {
                console.log(error);
            });
    },

    getDb: function () {
        return database;
    },
};