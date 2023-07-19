const mongoose = require("mongoose");
const uniqueValidator = require("mongoose-unique-validator");

const Schema = mongoose.Schema;

const ErrorSchema = new Schema({
    userId: { type: String, required: true },
    source: { type: String, required: true },
    message: { type: String, required: true },
    code: { type: String, required: true },
    type: { type: String, required: true },
    created: { type: String, required: true }
});

ErrorSchema.plugin(uniqueValidator);

module.exports = mongoose.model("Error", ErrorSchema);
