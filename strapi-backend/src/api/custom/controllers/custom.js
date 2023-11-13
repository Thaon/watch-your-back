"use strict";

const _ = require("lodash");
const jwt = require("jsonwebtoken");

const issue = (payload, jwtOptions = {}) => {
  _.defaults(jwtOptions, strapi.config.get("plugin.users-permissions.jwt"));
  return jwt.sign(
    _.clone(payload.toJSON ? payload.toJSON() : payload),
    strapi.config.get("plugin.users-permissions.jwtSecret"),
    jwtOptions
  );
};

module.exports = {
  login: async (ctx) => {
    const data = ctx.request.body;
    let user = null;

    // find a user with the loginID
    user = await strapi.query("plugin::users-permissions.user").findOne({
      where: { deviceID: data.deviceID },
    });

    if (!user) {
      // fetch the role
      const role = await strapi
        .query("plugin::users-permissions.role")
        .findOne({ where: { type: "authenticated" } });
      // create the user
      user = await strapi.query("plugin::users-permissions.user").create({
        data: {
          ...data,
          role: role.id,
          provider: "local",
          confirmed: true,
        },
        populate: true,
      });
    }

    // update the access with player
    let today = new Date();
    today.setHours(0, 0, 0, 0);

    // find access for today
    let todayAccess = await strapi.query("api::access.access").findOne({
      where: {
        createdAt: {
          $gte: today,
        },
      },
      populate: { players: true },
    });

    if (!todayAccess)
      todayAccess = await strapi
        .query("api::access.access")
        .create({ data: { players: [] } });

    let players = todayAccess.players || [];
    if (!players.find((p) => p.id == user.id)) {
      await strapi.query("api::access.access").update({
        where: { id: todayAccess.id },
        data: { players: players.concat(user.id) },
      });
    }

    // return the user
    const jwt = issue(_.pick(user, ["id"]));

    return ctx.send({
      jwt,
      player: user,
    });
  },

  updateUsername: async (ctx) => {
    let { user } = ctx.state;
    const { username } = ctx.request.body;

    //update user username
    user = await strapi.query("plugin::users-permissions.user").update({
      where: {
        id: user.id,
      },
      data: { username: username },
    });

    //return OK
    return ctx.send(user);
  },

  updateEmail: async (ctx) => {
    let { deviceID, email } = ctx.request.body;

    //find a user who is trying to verify the e-mail using the provided code
    let confirmingUser = await strapi
      .query("plugin::users-permissions.user")
      .findOne({
        where: { deviceID: deviceID },
        populate: true,
      });
    if (confirmingUser == null) {
      return ctx.send({ status: "Could not find Player" });
    }

    // check if the user already has an email
    if (confirmingUser.email && confirmingUser.email != "") {
      return ctx.send({ status: "Email already updated" });
    }

    //update user email
    await strapi.query("plugin::users-permissions.user").update({
      where: {
        id: confirmingUser.id,
      },
      data: { email: email },
    });

    //return OK
    return ctx.send({ status: "success" });
  },

  trackMilestone: async (ctx) => {
    const { user } = ctx.state;
    const { payload } = ctx.request.body;

    // we separate milestones by name
    let existingMilestone = await strapi
      .query("api::milestone.milestone")
      .findOne({
        where: {
          name: payload,
        },
        populate: { players: true },
      });

    // create a new milestone if one is not already present
    if (!existingMilestone) {
      existingMilestone = await strapi
        .query("api::milestone.milestone")
        .create({ data: { players: [user.id], name: payload } });
    } else {
      // update the players to include the current user
      let players = existingMilestone.players || [];
      if (!players.find((p) => p.id == user.id)) {
        await strapi.query("api::milestone.milestone").update({
          where: { id: existingMilestone.id },
          data: { players: players.concat(user.id) },
        });
      }
    }

    return ctx.send({ status: "success" });
  },
};
