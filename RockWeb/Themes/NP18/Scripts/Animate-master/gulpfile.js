var gulp    = require('gulp'),
    plugins = require('gulp-load-plugins')(),
    package = require('./package.json'),
    sass = require('gulp-sass'),
    Server = require('karma').Server;
    opn = require('opn');

var paths = {
    output : 'assets/scripts/dist/',
    scripts : [
        'assets/js/src/**/*.js',
        'test/spec/*.js',
    ],
    styles : [
        'assets/scss/**/*.scss'
    ],
    html : [
        '*.html'
    ]
};

var banner = [
  '/*! ',
  '<%= package.name %> ',
  'v<%= package.version %> | ',
  '(c) ' + new Date().getFullYear() + ' <%= package.author %> |',
  ' <%= package.homepage %>',
  ' */',
  '\n'
].join('');

/**
 * Create local server to let us see what we are doing
 */
gulp.task('connect', function() {
    plugins.connect.server({
        root: [__dirname],
        port: 8000,
        livereload: true
    });

    opn('http://localhost:8000/test/results/unit-tests.html');
    opn('http://localhost:8000');
});

/**
 * Fire browser reload on html change
 */
gulp.task('html', function () {
    gulp.src(paths.html)
        .pipe(plugins.connect.reload());
});

/**
 * Compile our styles, prefix the result, rename and move
 */
gulp.task('styles', function () {
    gulp.src(paths.styles)
        .pipe(plugins.sass().on('error', sass.logError))
        .pipe(plugins.autoprefixer('last 2 versions', '> 1%', 'ie 8'))
        .pipe(plugins.header(banner, { package : package }))
        .pipe(gulp.dest('assets/css/'))
        .pipe(plugins.csso())
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(plugins.header(banner, { package : package }))
        .pipe(gulp.dest('assets/css/'))
        .pipe(plugins.connect.reload());
});

/**
 * Add nice banner comment to script file, move it, rename it, uglify it
 */
gulp.task('scripts', function() {
    return gulp.src(paths.scripts[0])
        .pipe(plugins.plumber())
        .pipe(plugins.header(banner, { package : package }))
        .pipe(gulp.dest('assets/js/dist/'))
        .pipe(plugins.rename({ suffix: '.min' }))
        .pipe(plugins.uglify())
        .pipe(plugins.header(banner, { package : package }))
        .pipe(gulp.dest('assets/js/dist/'))
        .pipe(plugins.connect.reload());
});

/**
 * Make sure our JS doesn't suck balls
 */
gulp.task('lint', function () {
    return gulp.src(paths.scripts[0])
        .pipe(plugins.plumber())
        .pipe(plugins.eslint({
            configFile: '.eslintrc'
        }))
        .pipe(plugins.eslint.format());
});

/**
 * Run test once and exit
 */
gulp.task('test', function (done) {
    new Server({
        configFile: __dirname + '/test/karma.conf.js',
        singleRun: true
    }, done).start();
});

/**
 * Watch for file changes and re-run tests on each change
 */
 gulp.task('tdd', function (done) {
    new Server({
        configFile: __dirname + '/test/karma.conf.js'
    }, done).start();
});

 /**
 * Watch our JS and SCSS files
 */
gulp.task('watch', function() {
    gulp.watch(paths.scripts, ['scripts', 'lint', 'tdd']);
    gulp.watch(paths.styles, ['styles']);
    gulp.watch(paths.html, ['html']);
});


gulp.task('dev', ['lint', 'test', 'connect', 'watch']);
gulp.task('build', ['lint', 'scripts', 'test']);
gulp.task('default', ['dev']);