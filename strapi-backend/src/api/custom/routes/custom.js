module.exports = {
  routes: [
    {
      method: "POST",
      path: "/update-username",
      handler: "custom.updateUsername",
    },
    {
      method: "POST",
      path: "/update-email",
      handler: "custom.updateEmail",
    },
    {
      method: "POST",
      path: "/login",
      handler: "custom.login",
    },
    {
      method: "POST",
      path: "/track-milestone",
      handler: "custom.trackMilestone",
    },
  ],
};
