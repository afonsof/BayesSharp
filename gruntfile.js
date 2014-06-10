module.exports = function(grunt) {
  require('load-grunt-tasks')(grunt);
  require('time-grunt')(grunt);

  grunt.initConfig({
  
    pkg: grunt.file.readJSON('package.json'),
  
    nugetpush: {
        dist: {
            src: 'pub/BayesSharp.<%= pkg.version %>.0.nupkg'
        }
    },
    
    shell: {
        nugetpack: {
            options: {
                stdout: true
            },
            command: 'md pub & nuget pack BayesSharp/BayesSharp.csproj -Prop Configuration=Release -OutputDirectory pub'
        }
    },
    
    assemblyinfo: {
        options: {
            files: ['BayesSharp/BayesSharp.csproj', 'Tests/UnitTests/UnitTests.csproj'],
            info: {
                description: '<%= pkg.description %>', 
                configuration: 'Release', 
                company: '<%= pkg.author %>', 
                product: '<%= pkg.name %>', 
                copyright: 'Copyright © <%= pkg.author %> ' + (new Date().getYear() + 1900), 
                version: '<%= pkg.version %>.0', 
                fileVersion: '<%= pkg.version %>.0'
            }
        }
    },
    
    msbuild: {
        src: ['BayesSharp.sln'],
        options: {
            verbosity: 'minimal',
            projectConfiguration: 'Release',
            version: 4.0,
            targets: ['Clean', 'Rebuild'],
            stdout: true
        }
    },
    
    nunit: {
        options: {
            files: ['Tests/UnitTests/bin/Release/BayesSharp.UnitTests.dll']
        }
    }
    
  });
  grunt.registerTask('default', ['assemblyinfo', 'msbuild', 'nunit', 'shell:nugetpack']);
  grunt.registerTask('push', ['default', 'nugetpush']);
};