var gulp        = require('gulp');
var browserSync = require('browser-sync').create();
var less        = require('gulp-less');
var root = process.cwd() + '/RockWeb/Themes/';

// Static Server + watching scss/html files
gulp.task('serve', ['less'], function() {

    browserSync.init({ proxy: 'rock.dev' });

    gulp.watch(root + '**/*.less', ['less']);
    gulp.watch(root + '**/*.(aspx|lava)').on('change', browserSync.reload);
});

// Compile less into CSS & auto-inject into browsers
gulp.task('less', function() {
    return gulp.src(root + '**/theme.less')
        .pipe(less())
        .pipe(gulp.dest(function(f) {
          return f.base;
        }))
        // XXX add support for grouping media queries
        // XXX add support for auto-prefixer
        // XXX add support for minification
        .pipe(browserSync.stream());
});

gulp.task('default', ['serve']);
