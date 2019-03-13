var gulp = require('gulp'),
	concat = require('gulp-concat'),
	rename = require('gulp-rename'),
	uglify = require('gulp-uglify'),
	jsImport = require('gulp-js-import'),
	sourcemaps = require('gulp-sourcemaps'),
	less = require('gulp-less'),
	path = require('path'),
	cleanCSS = require('gulp-clean-css'),
	plumber = require('gulp-plumber'),
	server = require('gulp-server-livereload'),
	browserSync = require('browser-sync'),
	nunjucksRender = require('gulp-nunjucks-render');

var lessSrc = './Styles/[^_]*.less',
    lessDest = './Styles/',
    scriptsSrc = './Scripts/[^_]*.js',
    scriptsDest = './Scripts/compiled/';

gulp.task('default', ['scripts', 'styles', 'nunjucks', 'watch']);

// Scripts Task
gulp.task('scripts', function() {
  return gulp.src(scriptsSrc)
    .pipe(plumber())
    // Import Plugin Files
    .pipe(jsImport({hideConsole: true}))
    // Minify
    .pipe(uglify())
    // Export
    .pipe(gulp.dest(scriptsDest))
    .pipe(gulp.dest('./_compiled/scripts'))
    // Reload Browser
    .pipe(browserSync.reload({
      stream: true
    }));
});

// Styles Task
gulp.task('styles', function () {
	gulp.src(lessSrc)
  	.pipe(plumber())
  	.pipe(sourcemaps.init())
  	.pipe(less({
  		paths: [ path.join(__dirname, 'less', 'includes') ]
  	}))
  	.pipe(sourcemaps.write())
    .pipe(cleanCSS({compatibility: 'ie8'}))
  	.pipe(gulp.dest(lessDest))
    .pipe(gulp.dest('./_compiled/'))
  	.pipe(browserSync.reload({
    		stream: true
  	}));
});

// Compile Nunjucks Task
gulp.task('nunjucks', ['browserSync'], function() {

  // Gets .html and .nunjucks files in pages
  gulp.src('./Assets/Pages/**/*.+(html|nunjucks)')
  	.pipe(plumber())

  	// Renders template with nunjucks
	.pipe(nunjucksRender({
		path: ['_nunjucks-layouts','Assets/Pages','Assets/Lava']
  }))

	// output files in app folder
	.pipe(gulp.dest('./_compiled/'))

  .pipe(browserSync.reload({
    stream: true
  }));

});

// Browser Sync
gulp.task('browserSync', function() {
  browserSync.init({
    server: {
    	baseDir: './_compiled/'
    },
  })
})

// Watch Styles Task
gulp.task('watch', function(){
  gulp.watch('./Styles/*.less', ['styles']);
  gulp.watch('./Scripts/*.js', ['scripts']);
  gulp.watch('./Assets/Pages/*.html', ['nunjucks']);
  gulp.watch('./Assets/Lava/*.html', ['nunjucks']);
});
 
// Web Server Task
gulp.task('webserver', ['watch'], function() {
  gulp.src('./_compiled/')
    .pipe(server({
    	defaultFile: 'index.html',
    	livereload: true,
    	directoryListing: false,
    	open: true
    }));
});

// Copy Task (Moves theme files to Rock directory)
gulp.task('copy', function () {
    gulp.src(['package.json','./gulpfile.js'])
        .pipe(gulp.dest('../Rock/RockWeb/Themes/NewSpring'));
    gulp.src(['Assets/**/*','!Assets/Pages','!Assets/Pages/*'])
        .pipe(gulp.dest('../Rock/RockWeb/Themes/NewSpring/Assets'));
    gulp.src(['Layouts/**/*'])
        .pipe(gulp.dest('../Rock/RockWeb/Themes/NewSpring/Layouts'));
    gulp.src(['Scripts/**/*'])
        .pipe(gulp.dest('../Rock/RockWeb/Themes/NewSpring/Scripts'));
    gulp.src(['Styles/**/*'])
        .pipe(gulp.dest('../Rock/RockWeb/Themes/NewSpring/Styles'));
});