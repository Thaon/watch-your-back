'use strict';

/**
 * leaderboard service
 */

const { createCoreService } = require('@strapi/strapi').factories;

module.exports = createCoreService('api::leaderboard.leaderboard');
