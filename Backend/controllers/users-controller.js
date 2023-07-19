const { validationResult } = require("express-validator");
const HttpError = require("../models/http-error");
const User = require("../models/user");

// Create user
const createUser = async (req, res, next) => {
    // Validation
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return next(
            new HttpError("Validating user failed when creating!", 422, errors)
        );
    }

    // Add to database
    User.create(req.body)
        .then((user) => {
            res.status(200).json(user);
        })
        .catch((err) => {
            return next(
                new HttpError("Adding user failed!", 500, err)
            );
        });
};

// Get user
const getUser = async (req, res, next) => {
    User.findOne({ [req.params.property]: req.params.value })
        .exec()
        .then((user) => {
            res.status(200).json(user);
        })
        .catch((err) => {
            return next(
                new HttpError("Getting user failed!", 500, err)
            );
        });
};

// Remove a user
const deleteUser = async (req, res, next) => {
    User.deleteOne({ _id: req.params._id })
        .then((user) => {
            res.status(200).json('Ok');;
        })
        .catch((err) => {
            return next(
                new HttpError("Deleting user failed!", 500, err)
            );
        });
};

exports.createUser = createUser;
exports.getUser = getUser;
exports.deleteUser = deleteUser;