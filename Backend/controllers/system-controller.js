const { validationResult } = require("express-validator");
const HttpError = require("../models/http-error");
const Error = require("../models/error");

// Check
const check = async (req, res, next) => {
    res.status(200).json("Ok");
};

// Error
const error = async (req, res, next) => {
    // Validation
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return next(
            new HttpError("Validating error failed when creating!", 422, errors)
        );
    }

    // Add to database
    Error.create(req.body)
        .then((error) => {
            res.status(200).json(error);
        })
        .catch((err) => {
            return next(
                new HttpError("Adding error failed!", 500, err)
            );
        });
};

exports.check = check;
exports.error = error;