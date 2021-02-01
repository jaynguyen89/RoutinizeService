<?php
declare(strict_types=1);

namespace App\Test\TestCase\Model\Table;

use App\Model\Table\UserphotosTable;
use Cake\ORM\TableRegistry;
use Cake\TestSuite\TestCase;

/**
 * App\Model\Table\UserphotosTable Test Case
 */
class UserphotosTableTest extends TestCase
{
    /**
     * Test subject
     *
     * @var \App\Model\Table\UserphotosTable
     */
    protected $Userphotos;

    /**
     * Fixtures
     *
     * @var array
     */
    protected $fixtures = [
        'app.Userphotos',
    ];

    /**
     * setUp method
     *
     * @return void
     */
    public function setUp(): void
    {
        parent::setUp();
        $config = TableRegistry::getTableLocator()->exists('Userphotos') ? [] : ['className' => UserphotosTable::class];
        $this->Userphotos = TableRegistry::getTableLocator()->get('Userphotos', $config);
    }

    /**
     * tearDown method
     *
     * @return void
     */
    public function tearDown(): void
    {
        unset($this->Userphotos);

        parent::tearDown();
    }

    /**
     * Test validationDefault method
     *
     * @return void
     */
    public function testValidationDefault(): void
    {
        $this->markTestIncomplete('Not implemented yet.');
    }
}
