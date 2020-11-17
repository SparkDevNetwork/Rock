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
          crossorigin: 'anonymous',
        },
        {
          rel: 'stylesheet',
          href: 'https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css',
          crossorigin: 'anonymous',
        },
        {
          rel: 'stylesheet',
          href: 'https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css',
          crossorigin: 'anonymous',
        },
      ],
    },
    body: {
      scripts: [
        {
          src: 'https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js',
          crossorigin: 'anonymous',
        },
        {
          src: 'https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js',
          crossorigin: 'anonymous',
        },
      ],
    },
  },
  sections: [
    {
      name: 'Project Documentation',
      content: 'docs/project_introduction.md',
      sections: [
        {
          name: 'Pages',
          description: 'An optional set of components to control application views. These are traditionally useed with Vue Router.',
          components: 'src/pages/**/*.vue',
        },
        {
          name: 'Components',
          description: 'The components used in the project.',
          components: 'src/components/**/*.vue',
        },
      ],
    },
    {
      name: 'System Setup',
      description: 'Useful tips and scripts for setting up your system for Vue Development',
      content: 'docs/System_Setup/Documentation/system_setup.md',
      sections: [
        {
          name: 'Useful VS Code Extensions',
          description: 'A list of Useful VS Code Extensions',
          content: 'docs/System_Setup/Documentation/vscodextensions.md',
        },
      ],
    },

  ],
  // 
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
