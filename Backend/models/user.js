const mongoose = require("mongoose");
const uniqueValidator = require("mongoose-unique-validator");

const Schema = mongoose.Schema;

const UserSchema = new Schema({
    playerId: { type: String, required: true, unique: true },
    email: { type: String, required: true, unique: true },
    created: { type: String, required: true }
});

UserSchema.plugin(uniqueValidator);

module.exports = mongoose.model("User", UserSchema);
