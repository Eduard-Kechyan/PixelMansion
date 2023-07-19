const express = require("express");
const systemControllers = require("../controllers/system-controller");
const router = express.Router();

module.exports = router;

router.get("/", systemControllers.check);

router.post("/error", systemControllers.error);
