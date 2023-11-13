// @ts-nocheck
"use strict";

/**
 * leaderboard controller
 */

const { createCoreController } = require("@strapi/strapi").factories;

module.exports = createCoreController(
  "api::leaderboard.leaderboard",
  ({ strapi }) => ({
    find: async (ctx) => {
      let leaderboards = await strapi
        .query("api::leaderboard.leaderboard")
        .findMany({
          populate: { scores: { populate: true } },
        });

      leaderboards.forEach((leaderboard) => {
        // reformat scores
        let scores = leaderboard.scores || [];
        let formattedScores = [];
        scores.forEach((score) => {
          formattedScores.push({
            username: score.user.username,
            value: score.value,
          });
        });

        leaderboard.scores = formattedScores;
      });

      return leaderboards;
    },

    update: async (ctx) => {
      const { user } = ctx.state;
      const { id } = ctx.params;
      const { value } = ctx.request.body;

      // find leaderboard
      let leaderboard = await strapi
        .query("api::leaderboard.leaderboard")
        .findOne({ where: { id: id } });

      // add score
      await strapi.query("api::score.score").create({
        data: { value, user: user.id, leaderboard: leaderboard.id },
      });

      // return updated leaderboard
      leaderboard = await strapi.query("api::leaderboard.leaderboard").findOne({
        where: { id: id },
        populate: { scores: { populate: true } },
      });

      // reformat scores
      let scores = leaderboard.scores || [];
      let formattedScores = [];
      scores.forEach((score) => {
        formattedScores.push({
          username: score.user.username,
          value: score.value,
        });
      });

      leaderboard.scores = formattedScores;

      return leaderboard;
    },
  })
);
