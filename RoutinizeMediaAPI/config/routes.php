<?php

use Cake\Http\Middleware\CsrfProtectionMiddleware;
use Cake\Routing\Route\DashedRoute;
use Cake\Routing\RouteBuilder;

$routes->setRouteClass(DashedRoute::class);

$routes->scope('/', function (RouteBuilder $builder) {

    $builder->registerMiddleware('csrf', new CsrfProtectionMiddleware([
        'httpOnly' => true,
    ]));

    // $builder->applyMiddleware('csrf');
    // $builder->setExtensions(['json']);

    // $builder->resources('Image');
    
    $builder->connect('access/filter-result?result={result}', ['controller' => 'Access', 'action' => 'filterResult']);

    $builder->connect('avatar/save-avatar', ['controller' => 'Avatar', 'action' => 'saveAvatar']);
    $builder->connect('avatar/replace-avatar', ['controller' => 'Avatar', 'action' => 'replaceAvatar']);
    $builder->connect('avatar/remove-avatar', ['controller' => 'Avatar', 'action' => 'removeAvatar']);

    $builder->connect('photo/save-photos', ['controller' => 'Photo', 'action' => 'savePhotos']);
    $builder->connect('photo/remove-photos', ['controller' => 'Photo', 'action' => 'removePhotos']);

    $builder->connect('attachment/save-attachments', ['controller' => 'Attachment', 'action' => 'saveAttachments']);
    $builder->connect('attachment/remove-attachments', ['controller' => 'Attachment', 'action' => 'removeAttachments']);

    $builder->connect('directory/clean-empty-dirs', ['controller' => 'Directory', 'action' => 'cleanEmptyDirectories']);
    $builder->connect('directory/delete-user-data-unsafe', ['controller' => 'Directory', 'action' => 'deleteUserDirectoriesAndFilesUnsafe']);
    $builder->connect('directory/safe-delete-album', ['controller' => 'Directory', 'action' => 'safeDeleteAlbum']);

    $builder->fallbacks();
});
