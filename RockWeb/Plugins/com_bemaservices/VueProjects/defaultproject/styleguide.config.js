module.exports = {
  // set your styleguidist configuration here
  title: 'Default Style Guide',
  template: {
    head: {
      links: [
        {
          href: 'https://fonts.googleapis.com/css?family=Roboto:300,400,500,700|Material+Icons',
          rel: 'stylesheet',
        },
        {
          rel: 'stylesheet',
          href: 'https://use.fontawesome.com/releases/v5.8.2/css/all.css',
          integrity: 'sha384-hWVjflwFxL6sNzntih27bfxkr27PmbbK/iSvJ+a4+0owXq79v+lsFkW54bOGbiDQ',
          crossorigin: 'anonymous',
        },
        {
          rel: 'stylesheet',
          href: 'https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css',
          integrity: 'sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u',
          crossorigin: 'anonymous',
        },
        {
          rel: 'stylesheet',
          href: 'https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css',
          integrity: 'sha384-rHyoN1iRsVXV4nD0JutlnGaslCJuC7uwjduW9SVrLvRYooPp2bWYgmgJQIXwl/Sp',
          crossorigin: 'anonymous',
        },
      ],
    },
    body: {
      scripts: [
        {
          src: 'https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js',
          integrity: 'sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa',
          crossorigin: 'anonymous',
        },
      ],
    },
  },
  sections: [
    {
      name: 'System Setup',
      description: 'Useful tips and scripts for setting up your system for Vue Development',
      content: 'docs/System_Setup/system_setup.md',
      sections: [
        {
          name: 'Useful VS Code Extensions',
          description: 'A list of Useful VS Code Extensions',
          content: 'docs/System_Setup/vscodextensions.md',
        },
      ]
      ,
    },
    {
      name: 'Project Documentation',
      content: 'docs/project_introduction.md',
      sections: [
      ],
    },
  ],
  // components: 'src/components/**/*.vue',
  defaultExample: true,
  // sections: [
  //   {
  //     name: 'First Section',
  //     components: 'src/components/**/*.vue'
  //   }
  // ],
  // webpackConfig: {
  //   // custom config goes here
  // },
  exampleMode: 'expand',
};
