const express = require("express");
const usersControllers = require("../controllers/users-controller");
const router = express.Router();

module.exports = router;

router.post("/", usersControllers.createUser);

router.get("/:property/:value", usersControllers.getUser);

router.delete("/:_id", usersControllers.deleteUser);
